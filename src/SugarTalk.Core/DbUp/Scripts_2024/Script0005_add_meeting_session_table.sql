alter table `meeting_user_session` add column `picture_url` varchar(512) null;
ALTER TABLE `meeting_user_session` ADD COLUMN `online_type` int NOT NULL DEFAULT 0;