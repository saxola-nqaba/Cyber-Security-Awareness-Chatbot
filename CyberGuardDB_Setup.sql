CREATE DATABASE IF NOT EXISTS cyberguard_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
 
USE cyberguard_db;

CREATE TABLE IF NOT EXISTS Tasks (
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    Title           VARCHAR(200)    NOT NULL,
    Description     VARCHAR(500)    NOT NULL DEFAULT '',
    ReminderDate    DATETIME        NULL,          -- NULL means no reminder set
    IsCompleted     TINYINT(1)      NOT NULL DEFAULT 0,
    CreatedAt       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP
);

DESCRIBE Tasks;

