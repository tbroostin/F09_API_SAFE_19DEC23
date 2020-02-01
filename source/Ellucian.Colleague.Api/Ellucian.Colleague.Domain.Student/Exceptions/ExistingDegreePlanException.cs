// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Domain.Student.Exceptions
{
    public class ExistingDegreePlanException : System.Exception
    {
        private readonly int? _ExistingPlanId;
        public int? ExistingPlanId { get { return _ExistingPlanId; } }
        
        public ExistingDegreePlanException()
        {
        }

        public ExistingDegreePlanException(string message, int existingPlanId)
            : base(message)
        {
            _ExistingPlanId = existingPlanId;
        }
    }
}
