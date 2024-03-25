ALTER TABLE meeting_speech DROP COLUMN `translated_text`;

ALTER TABLE meeting_chat_voice_record ADD COLUMN `translated_text` TEXT NULL;