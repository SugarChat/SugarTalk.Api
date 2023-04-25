alter table `meeting` add `origin_adress` varchar(512) null;
alter table `meeting` add `meeting_mode` int NOT NULL default 0;

alter table `meeting_session` add `meeting_mode` int NOT NULL default 0;

alter table `meeting` DROP column `meeting_type`;
alter table `meeting` add `meeting_type` int NULL default 0;

alter table `meeting_session` DROP column `meeting_type`;
alter table `meeting_session` add `meeting_type` int NULL default 0;