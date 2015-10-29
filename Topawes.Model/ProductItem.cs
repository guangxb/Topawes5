﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TopModel
{
    public class ProductItem : IComparable<ProductItem>
    {
        /// <summary>
        /// 如522571681053
        /// </summary>
        [Display(Name = "ID")]
        public long Id { get; set; }

        /// <summary>
        /// 是否监控
        /// </summary>
        [Display(Name = "是否监控")]
        [LocalProperty]
        public bool Monitor { get; set; }

        [Display(Name = "自动上架")]
        public bool AutoUpshelf { get; set; }
        /// <summary>
        /// 如137883
        /// </summary>
        [Display(Name = "SpuId")]
        public string SpuId { get; set; }
        /// <summary>
        /// 如 QB便宜直充不客气（5个）
        /// </summary>
        [Display(Name = "名称")]
        public string ItemName { get; set; }
        /// <summary>
        /// 如 类型：QQ直充
        /// </summary>
        [Display(Name = "下标名")]
        [BsonIndex]
        public string ItemSubName { get; set; }

        /// <summary>
        /// 如  卖家代充 
        /// </summary>
        [Display(Name = "类型")]
        public string Type { get; set; }

        /// <summary>
        /// 面值如  5元 
        /// </summary>
        public string 面值 { get; set; }

        /// <summary>
        /// 进价 如  4.77元 
        /// </summary>
        [Display(Name = "进价")]
        public decimal 进价 { get; set; }

        /// <summary>
        /// 淘宝一口价 如 4.80元
        /// </summary>
        public decimal 一口价 { get; set; }

        public decimal 利润 { get; set; }

        /// <summary>
        /// 通过界面右键菜单或按钮设置的利润值
        /// </summary>
        public decimal 原利润 { get; set; }

        [Display(Name = "供应商")]
        public string SupplierId { get; set; }

        [Display(Name = "位置")]
        public string Where { get; set; }

        [Display(Name = "库存")]
        [LocalProperty]
        public int? StockQty { get; set; }

        [Display(Name = "更新时间")]
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// 同步供应商操作已提交
        /// </summary>
        [Display(Name = "同步利润")]
        public bool SyncProfitSubmited { get; set; }

        /// <summary>
        /// 改价操作已提交
        /// </summary>
        [Display(Name = "修改利润")]
        public bool ModifyProfitSubmitted { get; set; }

        public bool 正在上架 { get; set; }

        public bool 正在下架 { get; set; }


       

        /// <summary>
        /// 获取供应商
        /// </summary>
        /// <returns></returns>
        public SuplierInfo GetSuplierInfo()
        {
            return new SuplierInfo
            {
                profitMax = this.利润,
                profitMin = this.利润,
                profitData = new SuplierDetail[] {
                        new SuplierDetail {
                            id =this.SupplierId,
                            price = this.进价
                        }
                }
            };
        }

        /// <summary>
        /// 更新供应商信息
        /// </summary>
        /// <param name="productItems"></param>
        /// <param name="supplier"></param>
        public void OnSupplierInfoUpdate(LiteCollection<ProductItem> productItems, SuplierInfo supplier)
        {
            this.利润 = supplier.profitMax;
            this.进价 = supplier.profitData[0].price;
            this.一口价 = supplier.profitData[0].price + supplier.profitMax;
            this.SupplierId = supplier.profitData[0].id;
            this.UpdateAt = DateTime.Now;
            productItems.Update(this);
        }

        public int CompareTo(ProductItem other)
        {
            return (int)(this.Id - other.Id);
        }

        /// <summary>
        /// 是否需要恢复价格
        /// </summary>
        [Display(Name = "恢复价格")]
        public bool NeedResotreProfit
        {
            get
            {
                return this.利润 != this.原利润;
            }
        }
    }
}
