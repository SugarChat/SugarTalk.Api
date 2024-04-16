ALTER TABLE meeting_chat_room_setting ADD COLUMN `transpose` FLOAT NULL;

ALTER TABLE meeting_chat_room_setting ADD COLUMN `speed` FLOAT NULL;

ALTER TABLE meeting_chat_room_setting ADD COLUMN `style` INT NULL;

ALTER TABLE meeting_chat_room_setting ADD COLUMN `inference_record_id` INT NULL;

ALTER TABLE meeting_chat_room_setting DROP COLUMN `gender`;

ALTER TABLE meeting_chat_voice_record ADD COLUMN `inference_record_id` INT NULL;