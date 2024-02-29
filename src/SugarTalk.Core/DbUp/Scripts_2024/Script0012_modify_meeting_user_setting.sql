alter table meeting_user_setting 
    add column `voice_id` varchar(48) not null,
    add column `language_id` int not null;

alter table meeting_speech_voice_table
    modify `language_id` int not null;