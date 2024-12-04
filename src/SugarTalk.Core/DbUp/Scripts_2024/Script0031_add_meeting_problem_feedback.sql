create table if not exists `meeting_problem_feedback`
(
    `id` INT NOT NULL AUTO_INCREMENT,
    `creator` NVARCHAR(128) NOT NULL,
    `category` INT NOT NULL,
    `description` NVARCHAR(2048) NOT NULL,
    `created_by` INT NOT NULL,
    `created_date` DATETIME(3) NOT NULL,
    `is_new` BIT NOT NULL,
    PRIMARY KEY(`id`)
    )charset=utf8mb4