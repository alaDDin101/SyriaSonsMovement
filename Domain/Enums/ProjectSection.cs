namespace SyriaSonsMovement.Domain.Enums;

/// <summary>قسم المشروع ضمن صفحة «المشاريع والمبادرات».</summary>
public enum ProjectSection : byte
{
    /// <summary>ماذا تقدم الحركة عملياً</summary>
    WhatWeOffer = 1,

    /// <summary>مشاريع خدمية أو توعوية</summary>
    ServiceOrAwareness = 2,

    /// <summary>خطط مستقبلية</summary>
    FuturePlans = 3,
}
