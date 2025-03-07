alter table `meeting_participant`
    CHANGE column `third_party_user_id` `staff_id` varchar(128) not null;