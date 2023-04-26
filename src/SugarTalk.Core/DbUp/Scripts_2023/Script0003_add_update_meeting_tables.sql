drop table `meeting_session`;
drop table `user_session`;
drop table `user_auth_token`;

alter table `meeting` DROP column `meeting_type`;
alter table `meeting` add `origin_address` varchar(512) null;
alter table `meeting` add `meeting_stream_mode` int NOT NULL default 0;
