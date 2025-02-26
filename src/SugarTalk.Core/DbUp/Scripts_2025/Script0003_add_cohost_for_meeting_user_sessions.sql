alter table `meeting_user_session`
    add column `co_host` tinyint(1) null default 0;

alter table `meeting_user_session`
    add column `last_modified_date_for_co_host` datetime(3) null;

alter table `meeting`
    add column `created_by` int not null;

update `meeting` set `created_by` = `meeting_master_user_id`
