alter table `meeting`
    add column `is_waiting_room_enabled` tinyint(1) not null default 0;

alter table `meeting_user_session`
    add column `is_entry_meeting` tinyint(1) not null default 1;

alter table `meeting_user_session`
    add column `allow_entry_meeting` tinyint(1) not null default 1;