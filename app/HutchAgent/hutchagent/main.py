import sys
import logging
import pika
import hutchagent.message_queue as mq
import hutchagent.config as hutch_config
from hutchagent.db_manager import SyncDBManager
from hutchagent.db_logging import SyncLogDBHandler
from hutchagent.checkin import CheckIn
from hutchagent.query import query_callback


async def async_main():
    """An asynchronous version of the main function"""
    pass


def main():
    """The main method"""
    format = logging.Formatter(
        hutch_config.MSG_FORMAT,
        datefmt=hutch_config.DATE_FORMAT,
    )

    # set up the backup logger
    backup_handler = logging.StreamHandler(sys.stdout)
    backup_handler.setFormatter(format)
    backup_logger = logging.getLogger(hutch_config.BACKUP_LOGGER_NAME)
    backup_logger.setLevel(logging.INFO)
    backup_logger.addHandler(backup_handler)

    try:
        log_db_connection_str = hutch_config.get_log_db_conn()
    except Exception as e:
        backup_logger.critical(str(e))
        exit()

    # set up the db logger
    db_manager = SyncDBManager(log_db_connection_str)
    db_handler = SyncLogDBHandler(db_manager, hutch_config.BACKUP_LOGGER_NAME)
    db_handler.setFormatter(format)
    db_logger = logging.getLogger(hutch_config.DB_LOGGER_NAME)
    db_logger.setLevel(logging.INFO)
    db_logger.addHandler(db_handler)
    db_logger.addHandler(backup_handler)

    # set up check-in thread
    check_in_thread = CheckIn(cron=hutch_config.CRON_STRING)

    # Connect to RabbitMQ
    try:
        check_in_thread.start()
        db_logger.info("Connecting to queue.")
        channel = mq.connect(hutch_config.QUEUE_NAME)
        channel.basic_consume(
            hutch_config.QUEUE_NAME, on_message_callback=query_callback, auto_ack=True
        )
        db_logger.info("Successfully connected to queue. Press Ctrl+C to exit.")
        channel.start_consuming()  # starts a `while True` loop.
    except pika.exceptions.AMQPConnectionError:
        db_logger.critical("Unable to connect to queue. Exiting...", exc_info=True)
    except KeyboardInterrupt:
        # shut down on Ctrl+C
        db_logger.info("Disconnecting from queue...")
        if channel.connection.is_open:
            mq.disconnect(channel)

    if check_in_thread.is_alive():
        # stop check-in thred
        check_in_thread.join()

    db_logger.info("Successfully shut down :)")


if __name__ == "__main__":
    main()
