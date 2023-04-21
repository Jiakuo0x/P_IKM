using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Enums;

public enum BestSignDataType
{
    Unknown = 0,
    DescriptionFields = 10,
    RoleACompanyName = 20,
    RoleAAccount = 21,
    RoleBCompanyName = 30,
    RoleBAccount = 31,
    SenderAccount = 40,
    AStampHere = 50,
    BStampHere = 60,
    Tab_eStampRequire = 200,
}
