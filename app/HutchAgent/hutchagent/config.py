import requests


# Logging configuration
DB_LOGGER_NAME = "db"
BACKUP_LOGGER_NAME = "backup"
MSG_FORMAT = "%(levelname)s - %(asctime)s - %(message)s"
DATE_FORMAT = "%d-%b-%y %H:%M:%S"
LOG_TABLE_NAME = "Logs"  # need this for table definition in db_logging.py

# RabbitMQ configuration
QUEUE_NAME = "jobs"

# Manager related configuration
MANAGER_URL = "/path/to/manager"

# cron string
CRON_STRING = "0 */1 * * *"  # once every hour


def get_log_db_conn() -> str:
    """Get the connection string for the log database.

    Raises:
        Exception: Raised when the connection string isn't found.

    Returns:
        str: The connection string.
    """
    req = requests.get(f"{MANAGER_URL}/api/db-config")
    if config_dict := req.json:
        if conn := config_dict.get("connectionString"):
            return conn
    raise Exception("Could not retrieve log database connection string.")
