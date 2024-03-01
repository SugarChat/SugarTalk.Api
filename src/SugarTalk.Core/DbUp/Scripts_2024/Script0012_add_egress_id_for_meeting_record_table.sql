alter table `meeting_record` 
    add column `egress_id` varchar(128) null, 
    add column `url_status` int not null default 0;