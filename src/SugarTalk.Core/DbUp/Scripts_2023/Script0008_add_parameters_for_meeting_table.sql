alter table `meeting`
    add column `title` varchar(256) null,
    add column `time_zone` varchar(128) null,
    add column `security_code` varchar(128) null,
    add column `period_type` int default 0,
    add column `is_muted` tinyint(1) default 0 null,
    add column `is_recorded` tinyint(1) default 0 null,
    add column `is_active` tinyint(1) default 0 null;
    