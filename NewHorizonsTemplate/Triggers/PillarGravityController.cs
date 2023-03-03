using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DeepBramble.Triggers
{
    class PillarGravityController : MonoBehaviour
    {
        //Static things
        private static PillarGravityController activeField = null;
        private static PillarGravityController backupField = null;

        //Flags
        private bool alreadyEntered = false;
        private bool alreadyExited = true;

        /**
         * When the player enters, determine how this field should be treated
         * 
         * @param other The collider that entered
         */
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player") && !this.alreadyEntered)
            {
                DeepBramble.debugPrint(this + " called OnTriggerEnter");
                this.alreadyExited = false;
                this.alreadyEntered = true;

                //If there's no active field, set this one and allow it to align
                if (activeField == null)
                {
                    activeField = this;
                    this.gameObject.GetComponent<DirectionalForceVolume>()._affectsAlignment = true;
                    DeepBramble.debugPrint("Setting active field to " + this + " on trigger enter");
                }

                //There is an active field, set this to the backup
                else if(activeField != this)
                {
                    backupField = this;
                    DeepBramble.debugPrint("Setting backup field to " + this + " on trigger enter");
                }
            }
        }

        /**
         * When the player leaves, determine how this field should be treated
         * 
         * @param other The other collider
         */
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetAttachedOWRigidbody().CompareTag("Player") && !this.alreadyExited)
            {
                DeepBramble.debugPrint(this + " called OnTriggerExit");
                this.alreadyEntered = false;
                this.alreadyExited = true;

                //If this is the active field, demote it and promote the backup
                if (this == activeField)
                {
                    this.gameObject.GetComponent<DirectionalForceVolume>()._affectsAlignment = false;
                    if (backupField != null)
                    {
                        backupField.GetComponent<DirectionalForceVolume>()._affectsAlignment = true;
                    }
                    activeField = backupField;
                    backupField = null;
                    DeepBramble.debugPrint("Setting active field to " + activeField + " on trigger exit");
                }

                //If this is the backup field, clear it
                else if (this == backupField)
                {
                    backupField = null;
                    DeepBramble.debugPrint("Clearing backup field on trigger exit");
                }
            }
        }
    }
}
