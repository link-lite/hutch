import sys
import os
import pytest
import logging

from sqlalchemy import create_engine, select

from hutchagent.db_logging import Log, SyncLogDBHandler
from hutchagent.db_manager import SyncDBManager

CONNECTION_STRING_BASE = os.getenv("CONNECTION_STRING_BASE")
DB_NAME = os.getenv("DB_NAME")


@pytest.fixture
def spoof_db():
    ### SETUP
    # create the test database
    default_engine = create_engine(CONNECTION_STRING_BASE)
    with default_engine.connect() as conn:
        conn.execute("commit")  # a transaction is started by default - end it.
        conn.execute(f"create database {DB_NAME}")

    # provide an engine to interact with the test database
    test_engine = create_engine(CONNECTION_STRING_BASE + f"/{DB_NAME}")
    yield test_engine  # gives way to the test

    ### TEARDOWN
    # log out the test engine.
    test_engine.dispose()
    # drop the test database.
    with default_engine.connect() as conn:
        conn.execute("commit")  # a transaction is started by default - end it.
        conn.execute(f"drop database {DB_NAME}")


def test_sync_db_logger(spoof_db):
    # create the log table
    Log.metadata.create_all(spoof_db)

    format = logging.Formatter(
        "%(levelname)s - %(asctime)s - %(message)s",
        datefmt="%d-%b-%y %H:%M:%S",
    )

    # set up the backup logger
    backup_handler = logging.StreamHandler(sys.stdout)
    backup_handler.setFormatter(format)
    backup_logger = logging.getLogger("backup_logger")
    backup_logger.setLevel(logging.INFO)
    backup_logger.addHandler(backup_handler)

    # set up the db logger
    db_manager = SyncDBManager(connection_string=CONNECTION_STRING_BASE + f"/{DB_NAME}")
    db_handler = SyncLogDBHandler(db_manager, "backup_logger")
    db_handler.setFormatter(format)
    db_logger = logging.getLogger("db_logger")
    db_logger.setLevel(logging.INFO)
    db_logger.addHandler(db_handler)
    db_logger.addHandler(backup_handler)

    msg = "This is a test."
    expected_values = [(msg, "INFO"), (msg, "WARNING"), (msg, "CRITICAL")]

    db_logger.info("This is a test.")
    db_logger.warning("This is a test.")
    db_logger.critical("This is a test.")

    res = db_manager.execute_and_fetch(select(Log))
    assert len(res) == len(
        expected_values
    ), "Did not retrieve the expected number of results"
    for row, exp in zip(res, expected_values):
        assert row[1] == exp[0], "Message doesn't match expected message"
        assert row[3] == exp[1], "Log level doesn't match expected log level"
