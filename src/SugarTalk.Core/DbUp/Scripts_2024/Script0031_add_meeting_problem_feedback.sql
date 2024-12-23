create table if not exists `meeting_problem_feedback`
(
    `id` INT NOT NULL AUTO_INCREMENT,
    `categories` varchar(256) NOT NULL,
    `description` TEXT NOT NULL,
    `created_date` datetime(3) not null,
    `created_by` int not null,
    `last_modified_by` int not null,
    `last_modified_date` datetime(3) not null,
    PRIMARY KEY(`id`)
    )charset=utf8mb4;