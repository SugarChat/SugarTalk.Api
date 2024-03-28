ALTER TABLE meeting_chat_room_setting ADD COLUMN `transpose` INT NOT NULL;

ALTER TABLE meeting_chat_room_setting ADD COLUMN `speed` INT NOT NULL;

ALTER TABLE meeting_chat_room_setting DROP COLUMN `gender`;
