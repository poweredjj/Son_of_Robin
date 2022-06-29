using System;

namespace SonOfRobin
{
    public struct Highlighter
    {
        private readonly Object coupledObj;
        private readonly string coupledVarName;
        private readonly bool isOn;
        private bool isOnForOneFrame;
        public bool IsOn
        {
            get
            {
                if (this.coupledObj == null)
                {
                    bool onState = this.isOn || this.isOnForOneFrame;
                    this.isOnForOneFrame = false;

                    return onState;
                }
                else return (bool)Helpers.GetProperty(targetObj: coupledObj, propertyName: this.coupledVarName);
            }
        }

        public Highlighter(bool isOn, Object coupledObj = null, string coupledVarName = null)
        {
            this.isOn = isOn;
            this.coupledObj = coupledObj;
            this.coupledVarName = coupledVarName;
            this.isOnForOneFrame = false;
        }

        private void FailIfCoupledPrefIsSet()
        {
            if (this.coupledVarName != null) throw new ArgumentException("Cannot change highlight value, when coupled preference name is set.");
        }

        public void SetOnForNextRead()
        {
            this.FailIfCoupledPrefIsSet();
            this.isOnForOneFrame = true;
        }

    }
}
