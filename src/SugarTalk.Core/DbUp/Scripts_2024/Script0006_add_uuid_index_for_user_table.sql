CREATE INDEX idx_uuid ON user_account(uuid);

alter table `meeting` add column `creator_user_id` char(36) null;
alter table `meeting_speech` add column `user_entity_id` char(36) null;
alter table `meeting_user_setting` add column `user_entity_id` char(36) null;
alter table `meeting_user_session` add column `user_entity_id` char(36) null;