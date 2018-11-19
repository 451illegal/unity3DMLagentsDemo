using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RollerAgent : Agent {
       
    Rigidbody rB;

	// Use this for initialization
	void Start () {
		//get the rigidBody
        rB = GetComponent<Rigidbody>();
	}
	
    //get the target information
    public Transform target;
    
    /// <summary>
    /// //Agent重置函数
    /// 基本逻辑：当小球落地时，重置小球到平面上；
    /// 小球到达目标时会将自己标记为完成状态，而 agent 重置函数会将目标移动到随机位置
    /// </summary>
    public override void AgentReset()
    {
        //小球掉落平面
        if (this.transform.position.y < -1.0f)
        {
            //reset the position
            this.transform.position = Vector3.zero;
            //reset the rigidbody
            this.rB.angularVelocity = Vector3.zero;
            this.rB.velocity = Vector3.zero;
        }
        else
        {
            //随机移动target的位置
            target.position = new Vector3(Random.value * 8 - 4,
                                            0.5f,
                                            Random.value * 8 - 4);
        }
    }

    /// <summary>
    /// 收集观测信息
    /// </summary>
    List<float> observations = new List<float>();
    public override void CollectObservations()
    {
        //计算小球与目标的相对位置
        Vector3 relativePos = target.transform.position - this.transform.position;

        //添加观测数据(除以5为了使输入的值在[-1,1]范围内)
        AddVectorObs(relativePos.x / 5);
        AddVectorObs(relativePos.z / 5);
        //小球与平台边缘之间的距离
        AddVectorObs((this.transform.position.x + 5) / 5);
        AddVectorObs((this.transform.position.x - 5) / 5);
        AddVectorObs((this.transform.position.z + 5) / 5);
        AddVectorObs((this.transform.position.z - 5) / 5);
        //Agent的速度(有助于agent学习控制速度不至于太快滑落)
        AddVectorObs(this.rB.velocity.x / 5);
        AddVectorObs(this.rB.velocity.z / 5);

    }

    public float speed = 10f;
    private float previousDistance = float.MaxValue;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //通过与目标间的距离设置奖励
        float distanceToTarget = Vector3.Distance(this.transform.position, target.position);

        if (distanceToTarget < 1.42f)
        {
            //已到达目标,将agent标记为已完成状态
            Done();
            AddReward(1.0f);
        }
        if (distanceToTarget < previousDistance)
        {
            AddReward(0.1f);
        }
        //时间惩罚
        AddReward(0.05f);
        //小球掉落平台惩罚
        if (this.transform.position.y < -1.0f)
        {
            Done();
            AddReward(-1.0f);
        }
        previousDistance = distanceToTarget;

        //rigidbody move
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = Mathf.Clamp(vectorAction[0],-1,1);
        controlSignal.z = Mathf.Clamp(vectorAction[1],-1,1);
        rB.AddForce(controlSignal * speed);
    }



	// Update is called once per frame
	void Update () {
		
	}
}
