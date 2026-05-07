//背景图案
function ChangePattern(){
    var pattern=$("#SelectPattern").val();
    var url="./image/pattern/"+pattern+".svg";
    $("#Imageid").css({
        "background-image":"url("+url+")"
    });
}
//随机颜色
function GetRandomColor(){
	var rgb=(Math.random()*0xffffff<<0).toString(16);
	$("#Imageid").css("background-color","#"+rgb);
}
//利用chatgpt获取推荐颜色
function GetRecommendedColors(){
    var url="https://api.openai.com/v1/chat/completions";
    // var content="#F7B32B";
    // var array = content.match(/#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})/g);
    // console.log(array);
    // for(var i in array){
    //   var spancolor=document.createElement("span");
    //   spancolor.style.backgroundColor=array[i];
    //   spancolor.innerHTML=array[i];
    //   var RecommendationColors=document.getElementById("RecommendationColors");
    //   spancolor.setAttribute("class","RecommendationColor");
    //   RecommendationColors.appendChild(spancolor);
    // }
    if(strColor!=""){
      $("#RecommendationText").innerHTML="正在推荐……";
      $("#loading").css({
        "display":"block"
      });
      axios({
        method: 'post',
        url: url,
        headers:{
            "Authorization":"Bearer 你的Apikey"
        },
        data: {
            "model": "gpt-3.5-turbo",
            "messages": [{"role": "user", "content": "请推荐和"+strColor+"相配的颜色"}],
            "temperature": 0.7
        }
      }).then(function(response){
        console.log(response);
        var content=response.data.choices[0].message.content;
        var array = content.match(/#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})/g);
        console.log(array);
        for(var i in array){
          var spancolor=document.createElement("span");
          spancolor.setAttribute("class","RecommendationColor");
          spancolor.style.backgroundColor=array[i];
          spancolor.innerHTML=array[i];
          var RecommendationColors=document.getElementById("RecommendationColors");
          RecommendationColors.appendChild(spancolor);
        }
        $("#RecommendationText").innerHTML="智能推荐";
        $("#loading").css({
          "display":"none"
        });
      }).catch(function(error){
        console.log(error);
        $("#RecommendationText").innerHTML="智能推荐";
        $("#loading").css({
          "display":"none"
        });
      });
    }
}