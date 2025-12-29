alter table `meeting_record`
    add column `user_account_id` int null;

alter table `meeting_record`
    add column `original_id` varchar(36) null;

alter table `meeting_record`
    add column `display_title` varchar(255) null;