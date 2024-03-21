using System;
using Unity.Netcode;
using UnityEngine;

namespace HotPotato.Behaviours
{

    internal class CountdownExplosion : GrabbableObject
    {
        bool firstTimePickup = false; //Has this been picked up yet?
        private float beginAtCharge = 0.2f; //point where shaking should begin if battery charge is at this number

        bool disabled = false;
        bool exploded = false;

        float batteryCharge = 0.75f;

        FancyShake shakeTools; //This is where the shakin' ~MAGIC~ happens

        public override void Start()
        {
            base.Start();
            isBeingUsed = false;
            itemProperties.isConductiveMetal = true;
            itemProperties.requiresBattery = true;
            itemProperties.automaticallySetUsingPower = true;
            //itemProperties.batteryUsage = 165f;

            if (PotatoConfig.Instance.BATTERY_RANDOM)
            {
                changeBatteryServerRpc(); //Alright, we need to make a call to the server to set a random value
            }
            else
            {
                //Debug.Log("BATTERY RANDOMISER DISABLED");
                batteryCharge = 0.75f;
            }

            itemProperties.batteryUsage = PotatoConfig.Instance.BATTERY_USAGE;
            shakeTools = GetComponent<FancyShake>();
            insertedBattery = new Battery(false, batteryCharge);
            Debug.Log("Battery set to " + insertedBattery.charge);

            //If you managed to get into orbit or the game hasnt started yet and the potato is on ship at spawn, set it to disabled
            if (isInShipRoom)
            {
                Debug.Log("Potato already Disabled");
                disablePotatoServerRpc();
            }
            
            

            Debug.Log("HOT POTATO INITIALIZED");

        }




        [ServerRpc(RequireOwnership = false)]
        public void changeBatteryServerRpc()
        {
            float batteryValue;
            if(UnityEngine.Random.Range(0,101) <= 35) //Basically, 35% chance to be a high charge, 65% change to be a lower charge
            {
                batteryValue = UnityEngine.Random.Range(0.35f, 0.75f);
            }
            else
            {
                batteryValue = UnityEngine.Random.Range(0.01f, 0.35f);
            }
            //Debug.Log("Sending Battery Change to Clients-");
            changeBatteryClientRpc(batteryValue);
        }

        //Send the new value to all clients so it is consistent
        [ClientRpc]
        public void changeBatteryClientRpc(float newValue)
        {
            batteryCharge = newValue;
            //Debug.Log("BATTERY USAGE RANDOMISER ENABLED, VALUE IS " + batteryCharge);
            insertedBattery = new Battery(false, batteryCharge); //Just in case it needs to be set here
        }





        //Want to check with the server if a Hot Potato is already in the ship before the game starts, since isInShipRoom doesnt update for clients (idk WHY)
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("Hey fam, can you just check and see if values are right?");
            checkIfInShipServerRpc();
        }


        [ServerRpc(RequireOwnership = false)]
        public void checkIfInShipServerRpc()
        {
            if(isInShipRoom)
            {
                Debug.Log("Yeah, its in the Ship Room");
                disablePotatoServerRpc();
            }
            else
            {
                Debug.Log("Nah. Its not in the ship room");
            }
        }

        public override void Update()
        {
            base.Update();
            if (disabled) //If disabled, we want the battery to appear completely empty
            {
                insertedBattery.charge = 0;
                insertedBattery.empty = true;
                return;
            }


            if (firstTimePickup)
            {

                if(insertedBattery.charge <=0) //if the charge hits zero, its time to ZIM ZAM BOOZLE EXPLODE YOUR CANOODLE
                {
                    if (!exploded)
                    {
                        //Debug.Log("LEts Explode this bitch");
                        SetOffServerRpc(); 
                    }
                    else
                    {
                        Debug.Log("hey this potato should be removed.");
                    }
                }
                else if(insertedBattery.charge <= beginAtCharge) //This is when we start SHAKIN
                {
                    //Debug.Log("AIGHT SO IM SHAKIN");
                    beginShakeServerRpc(); 
                }
                else if (insertedBattery.charge >= 0.9f) //if the charge is basically full, then that means its hit a charging station, so we wanna disable
                {
                    //Debug.Log("HEY THE  CHARGE IS TOO DAMN HIGH");
                    disablePotatoServerRpc();
                }
            }
        }

        public override void PocketItem()
        {
            base.PocketItem();

            if(!disabled)
            GetComponentInChildren<SpriteRenderer>().enabled = false;
        }

        public override void EquipItem()
        {
            base.EquipItem();

            if(!disabled)
            GetComponentInChildren<SpriteRenderer>().enabled = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (disabled || exploded) return;
            base.ItemActivate(used, buttonDown);
            isBeingUsed = true; //This is to override the item being activated and turning off the item charge being used
        }

        //Want the potato to shake for everyone when its in someone's hand, not just the owner of the object (CAUSE THATS COOL YO)
        [ServerRpc(RequireOwnership = false)]
        public void beginShakeServerRpc() {
            //Debug.Log("AND SOOOOO WE SHAKIN SERVERSIDE");
            beginShakeClientRpc(); }

        [ClientRpc]
        public void beginShakeClientRpc()
        {
            //Debug.Log("YO IMMA LET YOU FINISH< BUT THIS CLIENT GOTTA SHAKE");
            shakeTools.Begin(this, beginAtCharge);
        }

        
        [ServerRpc(RequireOwnership = false)]
        public void disablePotatoServerRpc() {
            //Debug.Log("Is something calling disable potato server?");
            disablePotatoClientRpc(); }

        [ClientRpc]
        public void disablePotatoClientRpc()
        {
            Debug.Log("POTATO DISABLED");
            disabled = true;
            
            if(shakeTools != null && shakeTools.shouldShake == true)
                shakeTools.stopShake();

            GetComponentInChildren<SpriteRenderer>().enabled = false; //Gotta turn off hot it aint hot

            isBeingUsed = false;

            insertedBattery.charge = 0;
            insertedBattery.empty = true;

        }


        public override void GrabItem()
        {
            Debug.Log("First time pickup? " + firstTimePickup);
            base.GrabItem();
            if (disabled || exploded)
            {
                return;
            }
            //if it is your first time picking up the item, we wanna set this to true
            if (firstTimePickup == false) {
                Debug.Log("MAKIN THE FIRST TIME PICKUP TRUEEEE");
                firstTimePickup = true;
                GetComponentInChildren<SpriteRenderer>().enabled = true; //Gonna make HOT display on the screen
                UseItemBatteries();
            }

            
        }

        //This is just to make sure we can override the transforms and make the potato shake
        public override void LateUpdate()
        {
            try
            {
                if (shakeTools.shouldShake == false)
                {
                    if (parentObject != null)
                    {
                        base.transform.rotation = parentObject.rotation;
                        base.transform.Rotate(itemProperties.rotationOffset);
                        base.transform.position = parentObject.position;
                        Vector3 positionOffset = itemProperties.positionOffset;
                        positionOffset = parentObject.rotation * positionOffset;
                        base.transform.position += positionOffset;
                    }
                }
                else
                {
                    if (parentObject != null)
                    {
                        base.transform.rotation = parentObject.rotation;
                        base.transform.Rotate(itemProperties.rotationOffset);
                        base.transform.position = parentObject.position;
                        Vector3 positionOffset = itemProperties.positionOffset;
                        positionOffset = parentObject.rotation * positionOffset;
                        base.transform.position += positionOffset;
                        base.transform.position += new Vector3(0, 0, shakeTools.xyshake.y);
                    }
                }
            } catch(Exception e) { Debug.Log("Yeah fam u got - " + e); }

                if (radarIcon != null)
                {
                    radarIcon.position = base.transform.position;
                }
            
        }


        [ServerRpc(RequireOwnership = false)]
        public void SetOffServerRpc() {
            Debug.Log("YO WE GONNA DETONATE");
                SetOffClientRpc(); }

        [ClientRpc]
        public void SetOffClientRpc() {
            Debug.Log("CLIENTS SAY OK DETONATE");
            Detonate(); }

        public void Detonate()
        {
            if (disabled)
            {
                Debug.Log("Potato is disabled, cant detonate");
                return;
            }
            exploded = true;
            //potatoAudio.PlayOneShot(potatoBlast, 1f);
            //WalkieTalkie.TransmitOneShotAudio(potatoAudio, potatoBlast);
            Landmine.SpawnExplosion(base.transform.position + Vector3.up, spawnExplosionEffect: true, 5f, 8f);
            StunGrenadeItem.StunExplosion(base.transform.position + Vector3.up, true, 0.1f, 3.0f, 0.5f);

            grabbable = false;
            grabbableToEnemies = false;
            deactivated = true;
            //RoundManager.Instance.PlayAudibleNoise(base.transform.position, 11f, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed, 941);
            if (base.IsOwner) //if you own the scrap, tell the server to destroy it, cause it explodided
            {
                RemoveScrapServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveScrapServerRpc()
        {
            Debug.Log("Despawning Hot Potato Object");
            NetworkObject.Despawn(true);
        }

        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            Debug.Log("ENEMY HAS PICKED UP POTATO");
            //Basically, this means that an enemy (probably a hoarder bug) has picked up the item
            if (disabled || exploded)
            {
                return;
            }
            //if the enemy is picking up a potato that has never been activated, we wanna set this to true
            if (firstTimePickup == false)
            {
                Debug.Log("MAKIN THE FIRST TIME PICKUP TRUEEEE");
                firstTimePickup = true;
                GetComponentInChildren<SpriteRenderer>().enabled = true; //Gonna make HOT display on the screen
                UseItemBatteries();
            }
        }

    }
}
