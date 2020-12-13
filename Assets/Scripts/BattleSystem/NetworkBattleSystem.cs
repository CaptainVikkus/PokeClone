using System;
using System.Collections;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using NetworkMessages;
using System.Text;
using UnityEngine.Assertions;

public class NetworkBattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    private NetworkBattleServer battleServer;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    // Networking Info
    string connectionID;
    string enemyID;
    string enemyTrainerName;
    bool connected = false;

    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;

    private void Start()
    {
        //battleServer = GetComponent<NetworkBattleServer>();

        Debug.Log("Enemy IP: " + BattleData.enemyID + " : " + serverPort);
        serverIP = BattleData.enemyID;
        enemyTrainerName = BattleData.playerName;

        //Setup Driver
        m_Driver = NetworkDriver.Create();
        var entrypoint = NetworkEndPoint.AnyIpv4;
        entrypoint.Port = serverPort;
        if (m_Driver.Bind(entrypoint) != 0)
            Debug.Log("Failed to bind to port " + serverPort);
        else
            m_Driver.Listen();

        //Setup Connection
        m_Connection = default(NetworkConnection);
        //var endpoint = NetworkEndPoint.LoopbackIpv4;
        //endpoint.Port = serverPort;
        var endpoint = NetworkEndPoint.Parse(serverIP, serverPort);
        m_Connection = m_Driver.Connect(endpoint);

        Debug.Log("Address:" + endpoint.Address);
        Assert.IsTrue(m_Connection.IsCreated);

        StartCoroutine(FindServer(endpoint));
    }

    private IEnumerator FindServer(NetworkEndPoint endpoint)
    {
        while (m_Connection.GetState(m_Driver) != NetworkConnection.State.Connected)
        {
            if (m_Connection.GetState(m_Driver) == NetworkConnection.State.Disconnected)
            { break; }
            //m_Connection = m_Driver.Connect(endpoint);
            yield return dialogBox.TypeDialog($" Connecting to battle.");
        }
        StartCoroutine(SetupBattle());
    }

    public void StartBattle()
    {

    }

    private IEnumerator SetupBattle()
    {
        //Set up player
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        //Set up opponent
        var baseMon = PokemonBase.ReadBaseMonString(BattleData.pokemonName);
        enemyUnit.Pokemon = new Pokemon(baseMon, BattleData.Lvl);
        enemyUnit.Pokemon.HP = BattleData.hp;
        enemyUnit.Setup();
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($" {enemyTrainerName}'s { enemyUnit.Pokemon.Base.Name} wants to battle.");

        //PlayerAction();
        state = BattleData.turn;
        if (state == BattleState.PlayerAction) { PlayerAction(); }
        else { }
    }
    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
    }
    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    public void HandleUpdate()
    {
        m_Driver.ScheduleUpdate().Complete();

        switch (state)
        {
            case BattleState.PlayerAction:
                HandleActionSelection();
                break;
            case BattleState.PlayerMove:
                HandleMoveSelection();
                break;
            case BattleState.EnemyMove:
                HandleEnemyAction();
                break;
        }

    }

    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentMove > 0)
            {
                --currentMove;
            }
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
            {
                currentMove += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (currentMove > 1)
            {
                currentMove -= 2;
            }
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!m_Connection.IsCreated)
            {
                return;
            }
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }

    void HandleEnemyAction()
    {

    }


    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        bool hit;
        if (move.PP == 0)
        {
            move = PokemonBase.DefaultMove;
        }
        else
        {
            move.PP--;
        }
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}!!");

        if (move.Base.Accuracy >= UnityEngine.Random.Range(0, 100))
        {
            playerUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1.0f);

            enemyUnit.PlayHitAnimation();

            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                CauseStatusEffect(move, playerUnit, enemyUnit);
            }
            else
            {
                var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
                yield return enemyHud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }
            hit = true;
        }
        else
        {
            yield return dialogBox.TypeDialog($" {enemyTrainerName}'s {enemyUnit.Pokemon.Base.Name} avoided the attack!!");
            hit = false;
        }

        //Send To Server
        SendMoveToServer(move, hit);

        if (enemyUnit.Pokemon.HP <= 0) // Win
        {
            yield return dialogBox.TypeDialog($" {enemyTrainerName}'s {enemyUnit.Pokemon.Base.Name} Fainted!!");
            enemyUnit.PlayFaintAnimation();

            yield return StartCoroutine(RewardPlayer());
            OnBattleOver(true);
        }
        else // keep going
        {
            state = BattleState.EnemyMove;
        }
    }

    IEnumerator PerformEnemyMove(Move move, bool hit)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($" {enemyTrainerName}'s {enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!!");

        if (hit)
        {
            enemyUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1.0f);

            playerUnit.PlayHitAnimation();

            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                CauseStatusEffect(move, enemyUnit, playerUnit);
            }
            else
            {
                var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
                yield return playerHud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} avoided the attack!!");
        }

        if (playerUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted!!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1)
        {
            yield return dialogBox.TypeDialog("A critical hit!!");
        }

        if (damageDetails.TypeEffectiveness > 1)
        {
            yield return dialogBox.TypeDialog("It's super effective!!");
        }
        else if (damageDetails.TypeEffectiveness < 1)
        {
            yield return dialogBox.TypeDialog("It wasn't very effective...");
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
            {
                --currentAction;
            }
        }

        dialogBox.UpdateActionSelectedAction(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Run
                OnBattleOver(false);
            }
        }
    }

    IEnumerator RewardPlayer()
    {
        int trained = 1;
        int effortYeild = enemyUnit.Pokemon.Base.EffortYeild;
        int enemyLevel = enemyUnit.Pokemon.Level;

        int expereinceGained = (int)(trained * effortYeild * enemyLevel / 7);
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expereinceGained} exp!!");
        playerUnit.Pokemon.exp += expereinceGained;

        if (playerUnit.Pokemon.exp >= playerUnit.Pokemon.exp2NextLevel)
        {
            playerUnit.Pokemon.Level++;
            playerUnit.Pokemon.exp = 0;
            playerUnit.Pokemon.exp2NextLevel = (int)Math.Pow(playerUnit.Pokemon.Level, 3);
            playerUnit.Pokemon.CalculateStats();
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} leveled up");
        }

    }
    void CauseStatusEffect(Move move, BattleUnit source, BattleUnit target = null)
    {
        if (move.Base.Effects.Boosts != null)
        {
            if (move.Base.Target == MoveBase.MoveTarget.Self)
            {
                source.Pokemon.ApplyStatus(move.Base.Effects.Boosts);
            }
            else
            {
                target.Pokemon.ApplyStatus(move.Base.Effects.Boosts);
            }
        }
    }

    //void SendMoveToServer(Move move, bool hit)
    //{
    //    Debug.Log("Sent move to Battle Server");
    //    SendMoveToClient(move, hit, m_Connection);
    //}

    void SendMoveToServer(Move move, bool hit)
    {
        var movemsg = new MoveMessage();
        movemsg.MoveName = move.Base.name;
        movemsg.hit = hit;

        string message = JsonUtility.ToJson(movemsg);
        var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    void SendHeartBeat()
    {
        var hb = new MessageHeader();
        hb.type = MessageType.HEARTBEAT;
        var message = JsonUtility.ToJson(hb);

        var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    void OnData(DataStreamReader stream)
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        MessageHeader header = JsonUtility.FromJson<MessageHeader>(recMsg);

        switch (header.type)
        {
            case MessageType.MOVE_MSG: //Move Received
                var mMsg = JsonUtility.FromJson<MoveMessage>(recMsg);
                MoveBase mBase = MoveBaseList.GetMoveBase(mMsg.MoveName);
                Move move = new Move(mBase);
                Debug.Log("Move " + mMsg.MoveName + " Received in State" + state);
                if (state == BattleState.EnemyMove) { StartCoroutine(PerformEnemyMove(move, mMsg.hit)); }
                break;
            case MessageType.HEARTBEAT:
                Debug.Log("Hiya Neighbourino");
                break;
            default:
                Debug.Log("Unrecognized message received!");
                break;
        }
    }

    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        var c = m_Driver.Accept();
        if (c != default(NetworkConnection))
        {
            OnConnect(c);
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        cmd = m_Connection.PopEvent(m_Driver, out stream);
        while (cmd != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                OnConnect(m_Connection);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnData(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                OnDisconnect();
            }

            cmd = m_Connection.PopEvent(m_Driver, out stream);
        }
    }

    void OnConnect(NetworkConnection c)
    {
        Debug.Log("We are now connected to the Battle Server");
        m_Connection = c;
        connected = true;
        //StartCoroutine(Heartbeat());
    }
    void OnDisconnect()
    {
        Debug.Log("Client got disconnected from Battle Server");
        m_Connection.Disconnect(m_Driver);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }
}
