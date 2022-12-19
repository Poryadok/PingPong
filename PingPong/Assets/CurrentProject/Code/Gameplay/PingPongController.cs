using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace PM.PingPong.Gameplay
{
    public class PingPongController : NetworkBehaviour
    {
        public ulong LastPlayerHit;
        public bool IsRoundStarted;
        public bool IsGameOver;

        public int[] Score = new int[2];
        public event System.Action OnGoal;
        public event System.Action<ulong> OnWin;
        public event System.Action<int> OnCountdown;

        public event System.Action OnQuitRequest;

        public Ball Ball;
        public PlayerRocket RocketBlue;
        public PlayerRocket RocketRed;

        [SerializeField]
        private NetworkObject ballPrefab;

        [SerializeField]
        private NetworkObject rocketPrefab;

        [SerializeField]
        private List<ModeSettings> PingPongSettings;

        [SerializeField]
        private List<AbBonus> BonusPrefabs;

        private AbBonus SpawnedBonus;
        private Coroutine SpawnBonusCoroutine;
        
        private PingPongBalance balance;

        private List<ulong> playerIds;

        public void StartGame()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                Debug.LogError("Trying to start game from client");
                return;
            }

            balance = PingPongSettings.Find(x => (int)x.Difficulty == PlayerPrefs.GetInt("Difficulty")).Balance;

            var networkManager = NetworkManager.Singleton;
            playerIds = networkManager.ConnectedClients.Keys.ToList();

            var rocketBlueNO = Instantiate(rocketPrefab, Vector3.back * balance.RocketsZPos, Quaternion.identity);
            rocketBlueNO.SpawnAsPlayerObject(playerIds[0], true);
            rocketBlueNO.gameObject.name = "BlueRocket";
            rocketBlueNO.transform.localScale = new Vector3(5, 5, 5);

            RocketBlue = rocketBlueNO.GetComponent<PlayerRocket>();

            var rocketRedNO = Instantiate(rocketPrefab, Vector3.forward * balance.RocketsZPos,
                Quaternion.Euler(0, 180, 0));
            rocketRedNO.SpawnAsPlayerObject(playerIds[1], true);
            rocketRedNO.gameObject.name = "RedRocket";
            rocketRedNO.transform.localScale = new Vector3(5, 5, 5);

            RocketRed = rocketRedNO.GetComponent<PlayerRocket>();

            var ballNO = Instantiate(ballPrefab);
            ballNO.Spawn(true);

            Ball = ballNO.GetComponent<Ball>();
            Ball.Velocity = (Vector3.forward * 5 + Vector3.right * (UnityEngine.Random.value - 0.5f)).normalized *
                            balance.BallStartSpeed;
            Ball.Radius = balance.BallRadius;

            SetupRocketsClientRpc(PlayerPrefs.GetInt("Difficulty"));
        }

        [ClientRpc]
        private void SetupRocketsClientRpc(int difficulty)
        {
            balance = PingPongSettings.Find(x => (int)x.Difficulty == PlayerPrefs.GetInt("Difficulty")).Balance;

            // I'm not sure if package order is right, so wait until we have our rockets in place.
            StartCoroutine(SetupRockets());
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetClientReadyServerRpc(ulong clientId)
        {
            playerIds.Remove(clientId);

            if (playerIds.Count == 0)
            {
                StartCountDownClientRpc();
            }
        }

        [ClientRpc]
        private void StartCountDownClientRpc()
        {
            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            OnCountdown?.Invoke(3);
            yield return new WaitForSeconds(1f);
            OnCountdown?.Invoke(2);
            yield return new WaitForSeconds(1f);
            OnCountdown?.Invoke(1);
            yield return new WaitForSeconds(1f);
            OnCountdown?.Invoke(0);
            yield return new WaitForSeconds(0.2f);
            OnCountdown?.Invoke(-1);
            IsRoundStarted = true;
            SpawnBonusCoroutine = StartCoroutine(SpawnBonuses());
        }

        private IEnumerator SpawnBonuses()
        {
            yield return new WaitForSeconds(1f);

            while (IsRoundStarted && !IsGameOver)
            {
                if (SpawnedBonus == null)
                {
                    var randomPrefab = BonusPrefabs[UnityEngine.Random.Range(0, BonusPrefabs.Count)];
                    SpawnedBonus = Instantiate(randomPrefab, 
                        new Vector3(UnityEngine.Random.Range(0, balance.FieldWidth) - balance.FieldWidth / 2, 0, 0),
                        Quaternion.identity);
                    SpawnedBonus.GetComponent<NetworkObject>().Spawn(true);
                    SpawnedBonus.PingPongController = this;
                }
                
                yield return new WaitForSeconds(5f);
            }
        }

        private IEnumerator SetupRockets()
        {
            if (RocketBlue == null)
            {
                PlayerRocket[] rockets;
                do
                {
                    yield return null;
                    rockets = FindObjectsOfType<PlayerRocket>();
                } while (rockets.Length < 2);

                RocketBlue = Array.Find(rockets, x => x.transform.position.z < 0);
                RocketRed = Array.Find(rockets, x => x != RocketBlue);
            }

            RocketBlue.SetColor(Color.blue);
            RocketBlue.Size = balance.RocketStartSize;
            RocketBlue.WallsDistance = balance.FieldWidth / 2;
            RocketBlue.Acceleration = balance.RocketAcceleration;
            RocketBlue.Speed = balance.RocketStartSpeed;

            RocketRed.SetColor(Color.red);
            RocketRed.Size = balance.RocketStartSize;
            RocketRed.WallsDistance = balance.FieldWidth / 2;
            RocketRed.Acceleration = balance.RocketAcceleration;
            RocketRed.Speed = balance.RocketStartSpeed;

            SetClientReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        private void FixedUpdate()
        {
            if (!IsHost)
                return;

            if (!IsRoundStarted)
                return;

            if (IsGameOver)
                return;

            PingPongPhysics.SimulateBall(Ball, RocketBlue, RocketRed);

            if (Ball.Velocity.magnitude == 0)
            {
                IsRoundStarted = false;
                StopCoroutine(SpawnBonusCoroutine);
                DealWithGoal();
            }
            else if (SpawnedBonus != null && Vector3.Distance(Ball.transform.position, SpawnedBonus.transform.position) < Ball.Radius + SpawnedBonus.Radius)
            {
                SpawnedBonus.Activate();
                SpawnedBonus.GetComponent<NetworkObject>().Despawn();
            }
        }

        private IEnumerator TryFinishGame()
        {
            yield return new WaitForSeconds(2f);

            if (Score.Any(x => x >= 3))
            {
                if (Score[0] >= 3)
                {
                    CallTheWinClientRpc(NetworkManager.ConnectedClients.Keys.First());
                }
                else
                {
                    CallTheWinClientRpc(NetworkManager.ConnectedClients.Keys.Last());
                }
            }
            else
            {
                yield return StartNewRound();
            }
        }

        public void DealWithGoal()
        {
            if (Ball.transform.position.z > 0)
            {
                Score[0]++;
            }
            else
            {
                Score[1]++;
            }

            SetNewScoreClientRpc(Score);

            StartCoroutine(TryFinishGame());
        }

        public IEnumerator StartNewRound()
        {
            Ball.transform.position = Vector3.zero;
            Ball.Velocity =
                (Vector3.forward * (UnityEngine.Random.value > 0.5f ? 5 : -5) +
                 Vector3.right * (UnityEngine.Random.value - 0.5f)).normalized * balance.BallStartSpeed;

            IsRoundStarted = true;
            
            if (SpawnedBonus)
                SpawnedBonus.GetComponent<NetworkObject>().Despawn();
            
            RocketRed.Effects.Clear();
            RocketRed.UpdateScaleClientRpc(RocketRed.Size);
            RocketBlue.Effects.Clear();
            RocketBlue.UpdateScaleClientRpc(RocketBlue.Size);
            
            SpawnBonusCoroutine = StartCoroutine(SpawnBonuses());
            yield break;
        }

        public void LeaveGame()
        {
            OnQuitRequest?.Invoke();
        }

        [ClientRpc]
        private void SetNewScoreClientRpc(int[] score)
        {
            this.Score = score;
            OnGoal?.Invoke();
        }

        [ClientRpc]
        private void CallTheWinClientRpc(ulong winner)
        {
            IsGameOver = true;
            OnWin?.Invoke(winner);
        }
    }
}