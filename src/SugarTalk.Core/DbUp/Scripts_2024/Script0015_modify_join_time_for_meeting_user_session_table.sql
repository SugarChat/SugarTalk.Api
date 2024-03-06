ALTER TABLE `meeting_user_session` DROP COLUMN `first_join_time`;

alter table `meeting_user_session` add column `last_join_time` BIGINT null;
