using System;
using System.Collections;
using TracerLibrary;

namespace WorkflowLibrary
{
    /// <summary>
    /// Block class derived from a standard Activity
    /// </summary>
    public class Block : Activity, IActivity, IEnumerable, ICloneable
    {
        #region Fields

        private static int blockId;
        #endregion
        #region Constructors

        public Block() : base()
        {
            blockId = blockId + 1;
            _id = "block_" + blockId.ToString();
        }

        public Block(string id) : base(id)
        {
            _id = id;
			if (id.StartsWith("block_"))
            {
                if (blockId < Convert.ToInt16(this._id.Substring(6)))
                {
                    blockId = Convert.ToInt16(this._id.Substring(6));
                }
            }
        }

        #endregion Constructors
        #region Properties

        #endregion Properites
        #region Methods

        #endregion Methods
    }
}
