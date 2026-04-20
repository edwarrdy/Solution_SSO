using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
namespace Models
{
    /// <summary>
    /// 
    ///</summary>
    [SugarTable("sysuser")]
    public class Sysuser
    {
        
     
        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName="Id" ,IsPrimaryKey = true,IsIdentity = true) ]
        public int Id  { get; set;  } 
     
        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName="Username" ) ]
        public string? Username  { get; set;  } 
     
        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName="Password" ) ]
        public string? Password  { get; set;  } 
    

    }
    
}