﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.DAL.Repositories
{
    public class ComasysProductRepo<T> : BaseRepository<T> where T : class
    {
        public ComasysProductRepo(CRMContext context) : base(context)
        {

        }
    }
}
