<template>
<div id="app" style="width:1000px;margin: 0 auto;height:600px;transform: translate(0, 12%);">
    <Card style="height:740px;">
        <h1><span style="color:515a6e">Hi!&nbsp;&nbsp;&nbsp;</span><span v-text="callerName" style="color:#17233d" :callerKey='callerKey' ></span></h1>
        <Tabs v-model="tab">
            <TabPane label="公共聊天" name="pubilc">
                <div style="height:560px;overflow:auto;border:1px solid #dcdee2;" id='alldiv'>
                    <ul id="allmsg">
                        <li v-for="item in allmsg" :style="item.type==1||item.type==31?'text-align:center':''">
                            <Tag v-if="item.type==1" color="blue">
                                <span style="color:#808695">{{item.time}}</span>
                                <span title="@TA" @click="selectToUser(item.data.key,item.data.value)">{{item.data.value}}&nbsp;</span>
                                &nbsp;<span style="color:#808695">加入聊天</span>
                            </Tag>
                            <Tag v-else-if="item.type==31" color="gold">
                                <span style="color:#808695">{{item.time}}</span>
                                &nbsp;{{item.data.value}}
                                &nbsp;<span style="color:#808695">退出聊天</span>
                            </Tag>
                            <Tag type="dot" :color="callerKey==item.data.key?'blue':'primary'" v-else-if="item.type==4">
                                <span style="color:#808695">{{item.time}}</span>
                                <span title="@TA" @click="selectToUser(item.data.key,item.data.value)" style="color:#1890ff">&nbsp;{{item.data.value}}&nbsp;</span>
                                <span class="message">{{item.data.msg}}</span>
                            </Tag>
                        </li>
                    </ul>
                </div>
                <Card :padding='0' :bordered='false' :dis-hover='true' style="top:15px;height:50px;">
                    <Input search enter-button="发送" placeholder="输入聊天内容" v-model="message_text" @on-search="senAll"/>
                </Card>
            </TabPane>
            <TabPane label="个人聊天" name='private'>
                <div class="demo-split">
                    <div class="pane left">
                        <div style="height:600px;overflow:auto;">
                            <ul class="list" id="alluser">
                                <li>用户列表</li>
                                <li v-for="item in alluser" :title="'@'+item.value" class="list-view">
                                    <Avatar style="background:#7265e6">{{item.value[0]}}</Avatar>&nbsp;
                                    <span :data="item.key" @click="selectUser(item.key,item.value)">{{item.value}}</span>
                                </li>
                            </ul>
                        </div>
                    </div>
                    <div class="pane right">
                        <div class="pane top">
                            <div style="height:520px;overflow:auto;" id='privatediv'>
                                <ul id="privatemsg">
                                    <li v-for="item in privatemsg">
                                        <Tag type="dot" color="blue" v-if="item.type==61">
                                            <span style="color:#808695">{{item.time}}&nbsp;@</span>
                                            <span title="@TA" @click="selectUser(item.data.key,item.data.value)">{{item.data.value}}&nbsp;</span>
                                            <span class="message">{{item.data.msg}}</span>
                                        </Tag>
                                        <Tag type="dot" color="primary" v-if="item.type==6">
                                            <span style="color:#808695">{{item.time}}&nbsp;来自:</span>
                                            <span title="@TA" @click="selectUser(item.data.key,item.data.value)" style="color:#1890ff">{{item.data.value}}&nbsp;</span>
                                            <span class="message">{{item.data.msg}}</span>
                                        </Tag>
                                    </li>

                                </ul>
                            </div>
                        </div>
                        <div class="pane bottom">
                            <Card :padding='0' :bordered='false' :dis-hover='true' style="top:5px;height:50px;">
                                <Alert type="error">
                                    <span v-if="senduser">
                                          <span style="color:#808695">正在给</span>
                                    &nbsp;<span v-text="senduser" :key="sendkey" style="font-weight:bold;color:rgb(114, 101, 230)"></span>
                                    &nbsp;<span style="color:#808695">发送信息</span>
                                    </span>
                                    <span v-else>
                                        <span style="color:#808695">请在左侧选择要发送信息的人</span>
                                    </span>
                                </Alert>
                                <Input search enter-button="发送" placeholder="输入聊天内容" v-model="private_message_text" @on-search="sendprivatemsg" />
                            </Card>
                        </div>
                    </div>
                </div>
            </TabPane>
            <TabPane label="群组" name='group'>
                <div class="demo-split">
                    <div class="pane left">
                        <div style="height:600px;overflow:auto;">
                            <ul class="list" id="alluser">
                                <li>群列表
                                    <Button type="primary" icon="md-add-circle" size="small" style="margin-left: 40px;" @click="createNewGroup()">
                                        创建新群组
                                    </Button>
                                </li>
                                <li v-for="item in grouplist" class="list-view">
                                    <Avatar style="background:#7265e6">{{item[0]}}</Avatar>&nbsp;
                                    <span @click="selectGroup(item)">{{item}}</span>
                                </li>
                            </ul>
                        </div>
                    </div>
                    <div class="pane right">
                        <div class="pane top">
                            <div style="height:560px;overflow:auto;" id='groupdiv'>
                                <ul id="groupmsg">
                                    <li v-for="item in groupmsg" :style="item.type==51||item.type==52?'text-align:center':''">
                                        <Tag v-if="item.type==51" color="blue">
                                            <span style="color:#808695">{{item.time}}</span>
                                            <span title="@TA" @click="selectToUser(item.data.key,item.data.value)">&nbsp;{{item.data.value}}&nbsp;</span>
                                            &nbsp;<span style="color:#808695">加入
                                                【<span style="color:rgb(114, 101, 230)">&nbsp;{{item.data.group}}</span>】
                                            </span>

                                        </Tag>
                                        <Tag v-else-if="item.type==52" color="gold">
                                            <span style="color:#808695">{{item.time}}</span>
                                            &nbsp;{{item.data.value}}&nbsp;
                                            &nbsp;<span style="color:#808695">退出</span>
                                            【<span style="color:rgb(114, 101, 230)">&nbsp;{{item.data.group}}</span>】
                                        </Tag>
                                        <Tag type="dot" v-else-if="item.type==5" :color="callerKey==item.data.key?'blue':'primary'">
                                            <span style="color:#808695">{{item.time}}</span>
                                            <span style="color:rgb(114, 101, 230)">&nbsp;【{{item.data.group}}】</span>
                                            <span title="@TA" @click="selectToUser(item.data.key,item.data.value)" style="color:#1890ff">&nbsp;{{item.data.value}}&nbsp;</span>
                                            <span class="message">{{item.data.msg}}</span>
                                        </Tag>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <div class="pane bottom">
                            <Card :padding='0' :bordered='false' :dis-hover='true' style="">
                                <Alert type="error">
                                    <span v-if="sendgroup">
                                          <span style="color:#808695">正在给群组</span>
                                    &nbsp;<span v-text="sendgroup" :key="sendgroup" style="font-weight:bold;color:rgb(114, 101, 230)"></span>
                                    &nbsp;<span style="color:#808695">发送信息</span>
                                    <Button type="warning" size="small" @click="leave_group" icon="md-exit" style="float:auto;margin-top:0px;margin-right:-50px;">离开群组</Button>
                                    </span>
                                    <span v-else>
                                        <span style="color:#808695">请在左侧选择要发送信息的群</span>
                                    </span>
                                </Alert>
                                <Input search enter-button="发送" @on-search="sendgroupmsg" placeholder="输入聊天内容" v-model="group_message_text"/>
                            </Card>
                        </div>
                    </div>
                </div>
            </TabPane>
            <ul id="messagesList"></ul>
        </Tabs>
    </Card>
</div>
</template>

<script>
import * as signalR from '@aspnet/signalr'
export default {
    data() {
        return {
            tab: 'group',
            message_text: '',
            private_message_text: '',
            group_message_text: '',
            newgroupName: '',
            allmsg: [],
            privatemsg: [],
            groupmsg: [],
            alluser: [],
            grouplist: [],
            senduser: '',
            sendgroup: '',
            sendkey: '',
            callerName: '',
            callerKey: '',
            connection: null,
            modal1: false
        }
    },
    methods: {
        senAll() {
            if (this.message_text == '') {
                this.$Message.error('发送内容不能为空')
                return;
            }
            this.connection.invoke('Send', this.message_text)
            this.message_text = ''
        },
        leave_group() {
            this.$Modal.confirm({
                title: '提示',
                content: '确认退出群组【' + this.sendgroup + '】吗？',
                onOk: () => {
                    this.connection.invoke('LeaveGroup', this.sendgroup)
                    this.$Message.info('您已离开群组' + this.sendgroup);
                    this.sendgroup = ''
                }
            });
        },
        sendgroupmsg() {
            if (this.sendgroup.length == 0) {
                this.$Message.error('请先选择要发送的群')
                this.$Modal.warning({
                    title: '提示',
                    content: '请先在左侧选择要发送的群'
                })
                return
            }
            if (this.group_message_text == '') {
                this.$Message.error('发送内容不能为空')
                return
            }
            this.connection.invoke('SendToGroup', this.sendgroup, this.group_message_text)
            this.group_message_text = ''
        },
        sendprivatemsg() { //发送私聊信息
            if (this.sendkey.length < 3) {
                this.$Message.error('请先选择要发送的人')
                this.$Modal.warning({
                    title: '提示',
                    content: '请先在左侧选择要发送的人'
                })
                return
            }
            if (this.private_message_text == '') {
                this.$Message.error('发送内容不能为空')
                return
            }
            this.connection.invoke('SendPrivate', this.sendkey, this.private_message_text)
            this.private_message_text = ''
        },
        selectUser(k, u) {
            this.sendkey = k
            this.senduser = u
            this.$Message.info('@<span style="color:red;font-weight:bold">' + u + '</span>');
        },
        selectToUser(k, u) {
            if (k == this.callerKey) return;
            this.sendkey = k
            this.senduser = u
            this.tab = 'private'
            this.$Message.info('@<span style="color:red;font-weight:bold">' + u + '</span>');
        },
        selectGroup(v) {
            this.sendgroup = v
            this.connection.invoke('JoinGroup', v)
        },
        createNewGroup() {
            this.$Modal.confirm({
                render: (h) => {
                    return h('Input', {
                        props: {
                            value: this.newgroupName,
                            autofocus: true,
                            placeholder: '输入群名称'
                        },
                        on: {
                            input: (val) => {
                                this.newgroupName = val;
                            }
                        }
                    })
                },
                onOk: () => {
                    if (this.newgroupName.length == 0) {
                        this.$Message.error('请输入群名称')
                        return
                    }
                    this.connection.invoke('JoinGroup', this.newgroupName)
                    this.$Message.info('创建成功！');
                    this.sendgroup = this.newgroupName
                },
            })
        }
    },
    mounted() {
        this.connection.start();
        //接收公共信息
        this.connection.on('Send', msg => {
            this.allmsg.push(msg)
            setTimeout(() => {
                var sc = document.getElementById("alldiv")
                sc.scrollTop = sc.scrollHeight + 44
            }, 10)
        })
        //接收用户上下线通知
        this.connection.on('List', user => {
            console.info(JSON.stringify(user))
            if (user.type == 2) { //用户连接，获得列表
                for (var i = 0; i < user.data.length; i++) {
                    this.alluser.push(user.data[i])
                }
            } else if (user.type == 11) { //用户上线，新增列表
                this.alluser.push(user.data)
            } else if (user.type == 3) { //用户下线，列表移除
                for (var i = 0; i < this.alluser.length; i++) {
                    if (this.alluser[i].key == user.data.key) {
                        this.alluser.splice(i, 1)
                        break
                    }
                }
            } else if (user.type == 12) { //自己的信息
                this.callerName = user.data.value
                this.callerKey = user.data.key
            }
        })
        //接收私密信息
        this.connection.on('SendUser', msg => {
            this.privatemsg.push(msg)
            setTimeout(() => {
                var sc = document.getElementById("privatediv")
                sc.scrollTop = sc.scrollHeight + 44
            }, 10)
        })
        //接收群信息
        this.connection.on('SendGroup', msg => {
            console.info(JSON.stringify(msg))
            this.groupmsg.push(msg)
            setTimeout(() => {
                var sc = document.getElementById("groupdiv")
                sc.scrollTop = sc.scrollHeight + 44
            }, 10)
        })
        //更新群列表
        this.connection.on('GroupList', group => {
            console.info(JSON.stringify(group))
            if (group.type == 53) { //用户连接，获得列表
                for (var i = 0; i < group.data.length; i++) {
                    this.grouplist.push(group.data[i])
                }
            } else if (group.type == 54) { //新增群
                this.grouplist.push(group.data)
            } else if (group.type == 55) { //移除群
                for (var i = 0; i < this.grouplist.length; i++) {
                    if (this.grouplist[i] == group.data) {
                        this.grouplist.splice(i, 1)
                        break
                    }
                }
            }
        })
    },
    created() {
        this.connection = new signalR.HubConnectionBuilder().withUrl('http://localhost:12983/chatHub/').build();
    },
    beforeRouteLeave(to, from, next) {
        this.connection.stop()
    },
}
</script>

<style>
ul li {
    list-style: none;

}

.message {
    font-size: 14px;
    color: #000;
}

.list {
    margin-left: 10px;
}

.list li {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    font-family: "Helvetica Neue", Helvetica, "PingFang SC", "Hiragino Sans GB", "Microsoft YaHei", "微软雅黑", Arial, sans-serif;
    padding: 5px 0 3px 0;
    line-height: 1;
}

.list .list-view:hover {
    color: #3399ff;
    cursor: pointer;
}

.demo-split {
    height: 100%;
}

.pane {
    border-collapse: collapse;
}

.left {
    border-top: 1px solid #dcdee2;
    border-bottom: 1px solid #dcdee2;
    border-left: 1px solid #dcdee2;
    float: left;
    width: 20%;
    height: 95%
}

.right {
    float: right;
    width: 80%;
    height: 95%
}

.top {
    border: 1px solid #dcdee2;
    height: 85%;
}

.bottom {
    border-left: 1px solid #dcdee2;
    height: 15%;
}

.demo-split-pane {
    padding: 10px;
}

.demo-split-pane.no-padding {
    height: 200px;
    padding: 0;
}
</style>
