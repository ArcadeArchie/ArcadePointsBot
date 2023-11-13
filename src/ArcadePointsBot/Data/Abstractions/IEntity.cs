using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.Data.Abstractions;

public interface IEntity<TId>
{
    TId Id { get; }
}
