using System;

namespace SugarTalk.Core.Data.Exceptions;

public class MissingCurrentUserWhenSavingNonNullableFieldException : Exception
{
    public MissingCurrentUserWhenSavingNonNullableFieldException(string nonNullAbleField) : base($"保存非空字段{nonNullAbleField}时缺少操作人")
    {
    }
}