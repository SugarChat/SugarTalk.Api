alter table `meeting_repeat_rule` 
    add column `customize_repeat_type` int null;

alter table `meeting_repeat_rule` 
    add column `repeat_interval` int null;

alter table `meeting_repeat_rule` 
    add column `repeat_weekdays` varchar(512) null;

alter table `meeting_repeat_rule` 
    add column `repeat_month_days` varchar(512) null;
