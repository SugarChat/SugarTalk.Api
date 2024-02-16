ALTER TABLE `meeting_user_session` ADD COLUMN `is_meeting_master` int NOT NULL DEFAULT 0;
ALTER TABLE `meeting_user_session` ADD COLUMN `online_type` int NOT NULL DEFAULT 0;