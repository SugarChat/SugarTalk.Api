alter table `meeting_user_session`
    add column `co_host` tinyint(1) null default 0;

alter table `meeting_user_session`
    add column `last_modified_date_for_co_host` date(3) null;

alter table `meeting`
    add column `created_by` int null;