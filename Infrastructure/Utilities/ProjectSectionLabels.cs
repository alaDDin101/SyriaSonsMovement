using SyriaSonsMovement.Domain.Enums;

namespace SyriaSonsMovement.Infrastructure.Utilities;

internal static class ProjectSectionLabels
{
    public static string Arabic(ProjectSection section) => section switch
    {
        ProjectSection.WhatWeOffer => "ماذا تقدم الحركة عملياً",
        ProjectSection.ServiceOrAwareness => "مشاريع خدمية أو توعوية",
        ProjectSection.FuturePlans => "خطط مستقبلية",
        _ => "أخرى",
    };

    public static bool IsDefined(byte value) => Enum.IsDefined(typeof(ProjectSection), value);
}
