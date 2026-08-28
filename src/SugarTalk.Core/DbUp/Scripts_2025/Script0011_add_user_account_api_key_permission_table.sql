create table if not exists `user_account_api_key_permission`
(
    `id` int primary key auto_increment,
    `user_account_api_key_id` int not null,
    `permission_name` varchar(255) not null,
    `created_on` datetime not null,
    unique key `uq_api_key_permission` (`user_account_api_key_id`, `permission_name`),
    key `idx_api_key_permission_api_key_id` (`user_account_api_key_id`)
)
