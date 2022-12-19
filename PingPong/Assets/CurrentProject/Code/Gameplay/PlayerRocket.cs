using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PM.PingPong.Gameplay
{
    public class PlayerRocket : NetworkBehaviour
    {
        public float Velocity;
        public float Speed;
        public float Acceleration;
        public SpriteRenderer SpriteRenderer;
        public float WallsDistance;
        
        [SerializeField] private InputAction Input;

        public float Size
        {
            get
            {
                var size = defaultSize;

                foreach (var effect in Effects)
                {
                    if (effect.Type == BonusEffectTypes.SizeScale)
                    {
                        size *= effect.Value;
                    }
                }

                return size;
            }
            set
            {
                defaultSize = value;
                UpdateScaleClientRpc(Size);
            }
        }

        private float defaultSize = 5;
        
        private float inputMoveValue;

        public List<BonusEffect> Effects = new List<BonusEffect>();

        private void OnEnable()
        {
            if (!IsOwner) 
                return;
    
            Input.Enable();
        }
    
        private void OnDisable()
        {
            if (!IsOwner) 
                return;
    
            Input.Disable();
        }
    
        private void Start()
        {
            if (!IsOwner) 
                return;
    
            Input.Enable();
        }
    
        private void Update()
        {
            if (IsOwner)
            {
                inputMoveValue = Input.ReadValue<float>();
            }

            if (IsHost)
            {
                for (int i = 0; i < Effects.Count; i++)
                {
                    if (Time.time > Effects[i].Duration)
                    {
                        Effects.RemoveAt(i);
                        i--;
                        UpdateScaleClientRpc(Size);
                    }
                }
            }
        }
    
        private void FixedUpdate()
        {
            if (!IsOwner) 
                return;

            var targetSpeed = inputMoveValue > 0 ? Speed : inputMoveValue < 0 ? -1 * Speed : 0;

            if (Velocity != targetSpeed)
            {
                var acceleration = Velocity < targetSpeed ? Acceleration : -1 * Acceleration;
                
                var tickAcceleration = acceleration * Time.deltaTime;
                if (Mathf.Abs(Velocity - targetSpeed) < Mathf.Abs(tickAcceleration))
                {
                    Velocity = targetSpeed;
                }
                else
                {
                    Velocity += tickAcceleration;
                }
            }
            
            var position = this.transform.position;
            position += Vector3.right * (Velocity * Time.deltaTime);

            var maxX = WallsDistance + Size / 2;

            if (Mathf.Abs(position.x) > maxX)
            {
                Velocity = 0;
            }
            
            position = new Vector3(Mathf.Clamp(position.x, maxX * -1, maxX),
                position.y, position.z);
            this.transform.position = position;
        }

        [ClientRpc]
        public void UpdateScaleClientRpc(float size)
        {
            this.transform.localScale = new Vector3(size, 1, 1);
        }
        
        public void SetColor(Color color)
        {
            this.SpriteRenderer.color = color;
        }
    }
}