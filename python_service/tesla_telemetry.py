import getopt
import time
import teslajson
import sys
import datetime
from azure.storage.table import TableService, Entity

def log(connection, message):
	currentTime = time.gmtime();
	entry = Entity();
	entry.PartitionKey = '{0}{1:02d}{2:02d}'.format(currentTime.tm_year, currentTime.tm_mon, currentTime.tm_mday);
	entry.RowKey = str(time.time());
	entry.message = message;
	connection.insert_entity('TeslaTelemetryLog', entry);

def record_tesla_power_state(connection, table_name, state):
	currentTime = time.gmtime();
	entry = Entity();
	entry.PartitionKey = '{0}{1:02d}{2:02d}'.format(currentTime.tm_year, currentTime.tm_mon, currentTime.tm_mday);
	entry.RowKey = str(time.time());
	entry.state = state;
	connection.insert_entity(table_name, entry);

def tesla_data_request(connection, state):
	num_tries = 0;
	
	while (num_tries < 5):
		try:
			return connection.vehicles[0].data_request(state);
		except:
			num_tries+=1;
			time.sleep(30);

	raise Exception("Failed to access Tesla data.");
		
def tesla_timestamp_to_partition_key(tesla_time_stamp):
	utc_datetime = datetime.datetime.utcfromtimestamp(tesla_time_stamp / 1000);
	return '{0}{1:02d}{2:02d}'.format(utc_datetime.year, utc_datetime.month, utc_datetime.day);

try:
	opts, args = getopt.getopt(sys.argv[1:], 'hl:');
except getopt.GetoptError:
	print ('tesla_telemetry.py -l log_file');

opts;

for opt, arg in opts:
	if opt == '-h':
		print ('tesla_telemetry.py -l log_file');
		sys.exit();
	elif opt == '-l':
		log_file = arg;

connection_accesstoken = YOUR_ACCESS_TOKEN;

previous_latitude = 0;
previous_longitude = 0;

# 30 min
max_sleep = 1800;

# 5 second
moving_sleep = 5;

# 60 second
stop_sleep = 60;

# offline sleep
offline_sleep = 300;

# 3 min
sleep_incremental = 180;

sleep_time = stop_sleep;

not_moving_count = 0;

azure_table_service = TableService(account_name=YOUR_ACCOUNT_NAME, account_key=YOUR_KEY);

shouldReconnect = 1;

while 1:
	log(azure_table_service, "Starting a new session.");

	# Always get a new connection to get the updated vehicle state.
	if (shouldReconnect == 1):
		try:	
			connection = teslajson.Connection(access_token = connection_accesstoken);
		except:
			log(azure_table_service, "Failed to contact Tesla API");
			time.sleep(default_sleep);
			continue;

	vehicle_status = connection.vehicles[0];
	log(azure_table_service, 'vehicle_state:{0}'.format(vehicle_status['state']));
	record_tesla_power_state (azure_table_service, 'TeslaPowerState', vehicle_status['state']);
	
	if (vehicle_status['state'] != 'online'):
		if (sleep_time != max_sleep):
                        sleep_time += not_moving_count * sleep_incremental;
                        not_moving_count += 1;
                        if (sleep_time > max_sleep): sleep_time = max_sleep;
		time.sleep(sleep_time);
		shouldReconnect = 1;
		continue;

	try:
		vehicle_state = tesla_data_request(connection, "vehicle_state");
		charge_state = tesla_data_request(connection, "charge_state");
		drive_state = tesla_data_request(connection, "drive_state");
		climate_state = tesla_data_request(connection, "climate_state");
	except:
		log(azure_table_service, "Failed to access Tesla data.");
		time.sleep(default_sleep);
		# Reconnect.
		connection = teslajson.Connection(access_token = connection_token);
		continue;
	
	# Store states to Azure Tables.
	vehicle_entity = Entity();
	vehicle_entity.PartitionKey = tesla_timestamp_to_partition_key(vehicle_state['timestamp']);
	vehicle_entity.RowKey= str(vehicle_state['timestamp'] /1000 );
	vehicle_entity.odometer = vehicle_state['odometer'];
	vehicle_entity.car_version = vehicle_state['car_version'];
	vehicle_entity.homelink_nearby = vehicle_state['homelink_nearby'];
	vehicle_entity.locked = vehicle_state['locked'];
	vehicle_entity.last_autopark_error = vehicle_state['last_autopark_error'];
	vehicle_entity.valet_mode = vehicle_state['valet_mode'];
	vehicle_entity.api_version = vehicle_state['api_version'];
	azure_table_service.insert_entity('TeslaVehicleState', vehicle_entity);
	
	# For charge state, keep everything
	charge_state['PartitionKey'] = tesla_timestamp_to_partition_key(charge_state['timestamp']);
	charge_state['RowKey'] = str(charge_state['timestamp'] /1000 );
	azure_table_service.insert_entity('TeslaChargeState', charge_state);
	
	# For drive state, keep everything
	drive_state['PartitionKey'] = tesla_timestamp_to_partition_key(drive_state['timestamp']);
	drive_state['RowKey'] = str(drive_state['timestamp'] /1000 );
	azure_table_service.insert_entity('TeslaDriveState', drive_state);
	
	# For climate state, keep everything
	climate_state['PartitionKey'] = tesla_timestamp_to_partition_key(climate_state['timestamp']);
	climate_state['RowKey'] = str(climate_state['timestamp'] /1000 ); 
	azure_table_service.insert_entity('TeslaClimateState', climate_state);

	if ((previous_latitude != drive_state['latitude']) | (previous_longitude != drive_state['longitude'])): 
		# If moving
		log(azure_table_service, "Moving detected");
		print('{0},{1}\n'.format(time.strftime("%Y-%m-%d %H:%M:%S"), "Moving detected."));
		sleep_time = moving_sleep;	
		not_moving_count = 0;
		previous_latitude = drive_state['latitude'];
		previous_longitude = drive_state['longitude'];
		shouldReconnect = 0;
	else:
		# If not moving.
		log(azure_table_service, "Not moving.");
		print('{0},{1}\n'.format(time.strftime("%Y-%m-%d %H:%M:%S"), "Not moving."));
		if (not_moving_count == 0):
			sleep_time = stop_sleep;
		if (sleep_time < max_sleep):
			sleep_time += not_moving_count * sleep_incremental;
			not_moving_count += 1;
			if (sleep_time > max_sleep): sleep_time = max_sleep;
		shouldReconnect = 1;
	time.sleep(sleep_time);
		
