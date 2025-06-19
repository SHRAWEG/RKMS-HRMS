using Microsoft.IdentityModel.Tokens;

namespace Hrms.Common.Extensions
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeDigits<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int length)
        {
            return ruleBuilder
                .Matches("^[0-9]{" + length + "}$")
                .WithMessage("'{PropertyName}' must be " + length + " digits.");
        }

        public static IRuleBuilderOptions<T, string> MustBeLength<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int length)
        {
            return ruleBuilder
                .Must(value => value.Length < length)
                .WithMessage("'{PropertyName}' must be less than" + length + " characters.");
        }

        public static IRuleBuilderOptions<T, TDataType> MustBeValues<T, TDataType>(
            this IRuleBuilder<T, TDataType> ruleBuilder,
            List<TDataType> comparables)
        {
            return ruleBuilder
                .Must(value => comparables.Contains(value))
                .WithMessage("Invalid input value.");
        }

        public static IRuleBuilderOptions<T, byte> MustBeBinary<T>(
            this IRuleBuilder<T, byte> ruleBuilder)
        {
            return ruleBuilder
                .Must(value => value == 0 || value == 1)
                .WithMessage("'{PropertyName}' must be 0 or 1.");
        }

        public static IRuleBuilderOptions<T, TField> IdMustExist<T, TSource, TField>(
            this IRuleBuilder<T, TField> ruleBuilder,
            IQueryable<TSource> source, string? field = "Id")
        {
            return ruleBuilder
                .Must(value => source.Where(x => EF.Property<TField>(x, field).Equals(value)).SingleOrDefault() != null)
                .WithMessage("'{PropertyName}' is invalid.");
        }

        public static IRuleBuilderOptions<T, TField> MustBeUnique<T, TSource, TField>(
            this IRuleBuilder<T, TField> ruleBuilder,
            IQueryable<TSource> source,
            string field)
        {
            return ruleBuilder
                .Must(value => source.Where(x => EF.Property<TField>(x, field).Equals(value)).SingleOrDefault() == null)
                .WithMessage("'{PropertyName}' has already been taken");
        }

        public static IRuleBuilder<T, int> MustBeRegistered<T>(
            this IRuleBuilder<T, int> ruleBuilder,
            DbHelper dbHelper)
        {
            return ruleBuilder
                .Must(value => dbHelper.EmpRegistered(value))
                .WithMessage("Employee must be registered");
        }

        public static IRuleBuilder<T, int> MustBeActive<T>(
            this IRuleBuilder<T, int> ruleBuilder,
            DbHelper dbHelper)
        {
            return ruleBuilder
                .Must(value => dbHelper.EmpIsActive(value))
                .WithMessage("Employee must be active");
        }

        public static IRuleBuilderOptions<T, TElement> MustBeIn<T, TElement>(
            this IRuleBuilder<T, TElement> ruleBuilder,
            IEnumerable<TElement> elements)
        {
            return ruleBuilder
                .Must(value => elements.Contains(value))
                .WithMessage("'{PropertyName}' is invalid.");
        }

        public static IRuleBuilderOptions<T, string> MustBeDate<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(value => DateOnly.TryParseExact(value, "yyyy-MM-dd", out _))
                .WithMessage("'{PropertyName}' must be in the format yyyy-MM-dd.");
        }

        public static IRuleBuilderOptions<T, string> MustBeDateBefore<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return DateOnlyHelper.ParseDateOrNow(otherValue) > DateOnlyHelper.ParseDateOrNow(value);
                })
                .WithMessage("'{PropertyName}' must be before '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeDateBeforeOrEqual<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return DateOnlyHelper.ParseDateOrNow(otherValue) >= DateOnlyHelper.ParseDateOrNow(value);
                })
                .WithMessage("'{PropertyName}' must be before or equal to '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeDateAfter<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return DateOnlyHelper.ParseDateOrNow(otherValue) < DateOnlyHelper.ParseDateOrNow(value);
                })
                .WithMessage("'{PropertyName}' must be after '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeDateAfterOrEqual<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return DateOnlyHelper.ParseDateOrNow(otherValue) <= DateOnlyHelper.ParseDateOrNow(value);
                })
                .WithMessage("'{PropertyName}' must be after or equal to '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeDateBeforeNow<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    return DateOnlyHelper.ParseDateOrNow(value) < DateOnly.FromDateTime(DateTime.Now);
                })
                .WithMessage("'{PropertyName}' must be before today.");
        }
        
        public static IRuleBuilderOptions<T, string> MustBeDateAfterNow<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    return DateOnlyHelper.ParseDateOrNow(value) > DateOnly.FromDateTime(DateTime.Now);
                })
                .WithMessage("'{PropertyName}' must be after today.");
        }

        public static IRuleBuilderOptions<T, string> MustBeTime<T>(
            this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(value => TimeOnly.TryParseExact(value, "HH:mm:ss", out _))
                .WithMessage("'{PropertyName}' must be in the format HH:mm:ss.");
        }

        public static IRuleBuilderOptions<T, string> MustBeTimeBefore<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return TimeOnly.Parse(otherValue) > TimeOnly.Parse(value);
                })
                .WithMessage("'{PropertyName}' must be before '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeTimeBeforeOrEqual<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return TimeOnly.Parse(otherValue) >= TimeOnly.Parse(value);
                })
                .WithMessage("'{PropertyName}' must be before or equal to '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeTimeAfter<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return TimeOnly.Parse(otherValue) < TimeOnly.Parse(value);
                })
                .WithMessage("'{PropertyName}' must be after '" + otherPropertyName + "'.");
        }

        public static IRuleBuilderOptions<T, string> MustBeTimeAfterOrEqual<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            Expression<Func<T, string>> otherProperty,
            string otherPropertyName)
        {
            return ruleBuilder
                .Must((model, value) =>
                {
                    string otherValue = otherProperty.Compile().Invoke(model);

                    return TimeOnly.Parse(otherValue) <= TimeOnly.Parse(value);
                })
                .WithMessage("'{PropertyName}' must be after or equal to '" + otherPropertyName + "'.");
        }
    }
}
