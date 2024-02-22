alter table `meeting_record` add column `is_deleted` tinyint(1) null default 0;

alter table `meeting_record` modify `url` varchar(512) null;