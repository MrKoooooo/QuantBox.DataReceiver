% ��ȡh5����
data = h5read('test.h5','/IF1509_20150202');

% �ַ���Ҫת���ٴ���һ��
data.Symbol = cellstr(data.Symbol');
data.Exchange = cellstr(data.Exchange');

t = struct2table(data);

% �Ƚ�int32תint64
t.datetime = int64(t.ActionDay)*1000000 + int64(t.UpdateTime);

% ������ת���csv
writetable(t,'t.csv');