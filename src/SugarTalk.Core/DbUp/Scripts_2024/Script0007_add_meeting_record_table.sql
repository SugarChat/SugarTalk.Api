ALTER TABLE `meeting_record` ADD COLUMN `meeting_record_type` int NOT NULL DEFAULT 0;
ALTER TABLE `meeting_record` ADD COLUMN `egress_id` varchar(128) NOT NULL;