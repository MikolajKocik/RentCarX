using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Commands.Car.DeleteCar
{
    public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand>
    {
        private readonly IRentCarX_DbContext _context;

        public DeleteCarCommandHandler(IRentCarX_DbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Cars.FindAsync(new object[] { request.Id }, cancellationToken);
            if (entity == null) return Unit.Value;

            _context.Cars.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
