﻿using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.EntityBases.Concrete;

namespace Milvonion.Domain;

/// <summary>
/// Language entity.
/// </summary>
public class Language : LanguageEntity, ILanguage
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <returns></returns>
    public override object GetUniqueIdentifier() => Id;

    /// <summary>
    /// Returns this instance of "<see cref="Type"/>.Name <see cref="BaseEntity{TKey}"/>.Id" as string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{GetType().Name} {Id}]";
}