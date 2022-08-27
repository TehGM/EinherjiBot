namespace TehGM.EinherjiBot.API
{
    public static class ValidatatableExtensions
    {
        public static void ThrowValidateForCreation(this ICreateValidatable validatable)
            => ThrowErrors(validatable.ValidateForCreation());

        public static void ThrowValidateForUpdate<TEntity>(this IUpdateValidatable<TEntity> validatable, TEntity existing)
            => ThrowErrors(validatable.ValidateForUpdate(existing));

        private static void ThrowErrors(IEnumerable<string> errors)
        {
            if (errors?.Any() != true)
                return;

            throw new BadRequestException(errors);
        }
    }
}
