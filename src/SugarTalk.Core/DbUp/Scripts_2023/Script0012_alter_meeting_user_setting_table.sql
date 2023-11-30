alter table `meeting_user_setting` drop column `listened_language_type`;

alter table `meeting_user_setting`
    add column `meeting_id` varchar(36) null,
    add column `spanish_tone_type` int not null,
    add column `mandarin_tone_type` int not null,
    add column `english_tone_type` int not null,
    add column `cantonese_tone_type` int not null;