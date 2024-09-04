var MyIndexedDB = MyIndexedDB || {};

//创建数据表
MyIndexedDB.Create = function (dbName, version, objectStoreName) {
    //打开数据库
    var request = window.indexedDB.open(dbName, version);
    request.onerror = function (event) {
        console.log("数据库打开报错");
    }
    var db;
    request.onsuccess = function (event) {
        db = request.result;
        console.log("数据库打开成功");
    }
    //创建数据表
    request.onupgradeneeded = function (event) {
        db = event.target.result;
        console.log("数据库升级成功");
        if (!db.objectStoreNames.contains(objectStoreName)) {
            objectStore = db.createObjectStore(objectStoreName, { autoIncrement: true });
        }
    }
}

//添加数据
MyIndexedDB.Add = function (dbName, version, objectStoreName, object) {
    //打开数据库
    var db;
    var request = window.indexedDB.open(dbName, version);
    request.onerror = function (event) {
        console.log("数据库打开报错");
    }
    request.onsuccess = function (event) {
        db = request.result;
        console.log("数据库打开成功");
        //这里使用IndexedDB的巨坑之一，IndexedDB的方法均是异步，所以要在打开数据库连接的回调里执行CRUD操作
        var trans = db.transaction(objectStoreName, "readwrite");
        var objectStore = trans.objectStore(objectStoreName);
        var resultAdd = objectStore.add(object);
        resultAdd.onsuccess = function (event) {
            console.log("数据写入成功");
        }
        resultAdd.onerror = function (event) {
            console.log("数据写入失败");
        }
        resultAdd.onabort = function (event) {
            console.log("事务回滚");
        }
    }
}

MyIndexedDB.GetAll = function (dbName, version, objectStoreName) {
    return new Promise((resolve, reject) => {
        //打开数据库
        var db;
        var request = window.indexedDB.open(dbName, version);
        request.onerror = function (event) {
            console.log("数据库打开报错");
            reject(event); 
        }
        request.onsuccess = function (event) {
            db = request.result;
            console.log("数据库打开成功");
            const objectStore = db.transaction(objectStoreName).objectStore(objectStoreName);
            objectStore.getAll().onsuccess = (event) => {
                console.log(event.target.result);
                resolve(JSON.stringify(event.target.result));
            };
        }
    });
}