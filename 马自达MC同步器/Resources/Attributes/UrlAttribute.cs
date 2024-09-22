using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 马自达MC同步器.Resources.Attributes;

public sealed class UrlAttribute : ValidationAttribute
{
  public string PropertyName { get; }

  public UrlAttribute(string propertyName)
  {
    PropertyName = propertyName;
  }

  protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
  {
    var instance = validationContext.ObjectInstance;

    if (IsValidUrl(value.ToString())) return ValidationResult.Success;

    return new ValidationResult("Error Url");
  }

  private bool IsValidUrl(string? urlString)
  {
    return Uri.TryCreate(urlString, UriKind.Absolute, out var uriResult)
           && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
  }
}